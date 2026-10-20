namespace Anicluster.TagNetwork {

    /// <summary>Ein Knoten der Simulation (Position, Geschwindigkeit und Größe des Textfelds).</summary>
    public class ForceNode {

        public int Id { get; init; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Vx { get; set; }
        public double Vy { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        /// <summary>Inaktive Knoten werden nicht simuliert (ausgeblendete Tags).</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Festgehaltene Knoten (z. B. beim Ziehen) bewegen sich nicht selbst.</summary>
        public bool IsPinned { get; set; }

        /// <summary>Gewicht in der Abstoßung, größere Textfelder stoßen stärker ab.</summary>
        public double Size => (this.Width + this.Height) / 2.0 / 60.0;
    }

    /// <summary>Eine Verbindung zwischen zwei Knoten. Je stärker, desto näher rücken sie zusammen.</summary>
    public class ForceLink {

        public required ForceNode A { get; init; }
        public required ForceNode B { get; init; }

        /// <summary>Stärke von 0 bis 1 (Anteil der gemeinsamen Vorkommen am selteneren der beiden Tags)</summary>
        public double Strength { get; init; }
    }

    /// <summary>
    /// Kräftebasierte Simulation (Force-directed Layout), unabhängig von der Oberfläche:
    /// Alle Knoten stoßen sich ab, verbundene Knoten ziehen sich wie Federn an (stärker bei stärkerer Verbindung),
    /// eine schwache Kraft hält alles in der Mitte, Textfelder dürfen sich nicht überlappen.
    /// Die "Wärme" <see cref="Alpha"/> kühlt mit der Zeit ab, dann kommt die Bewegung zur Ruhe.
    /// Die Parameter wurden an synthetischen Daten mit 20 bis 80 Knoten abgestimmt.
    /// </summary>
    public class ForceSimulation {

        /// <summary>Ab dieser Wärme gilt die Simulation als beruhigt.</summary>
        public const double AlphaMin = 0.002;

        private const double Repulsion = 900.0;
        private const double LinkDistanceMin = 50.0;
        private const double LinkDistanceMax = 260.0;
        private const double LinkStrength = 0.18;
        private const double Gravity = 0.004;
        private const double Friction = 0.62;
        private const double Padding = 6.0;
        private const double Margin = 12.0;
        private const double MaxSpeed = 60.0;
        private const double MinDistanceSquared = 900.0;
        private const int CoolingTicks = 600;

        private readonly double _alphaDecay = 1.0 - Math.Pow(AlphaMin, 1.0 / CoolingTicks);
        private readonly Random _random = new Random(42);
        private readonly List<ForceNode> _active = [];

        public List<ForceNode> Nodes { get; } = [];

        public List<ForceLink> Links { get; } = [];

        /// <summary>Größe der Fläche, in der sich die Knoten bewegen</summary>
        public double Width { get; set; } = 900;

        public double Height { get; set; } = 600;

        /// <summary>Aktuelle Wärme von 1 (heiß, viel Bewegung) bis 0</summary>
        public double Alpha { get; private set; } = 1.0;

        public bool IsSettled => this.Alpha <= AlphaMin;

        /// <summary>Bringt Bewegung ins System, z. B. nach dem Ziehen oder Ausblenden eines Knotens.</summary>
        public void Reheat(double alpha = 0.7) {
            this.Alpha = Math.Min(1.0, Math.Max(this.Alpha, alpha));
        }

        /// <summary>Beginnt von vorn mit voller Wärme.</summary>
        public void Restart() {
            this.Alpha = 1.0;
        }

        /// <summary>Berechnet einen Schritt.</summary>
        public void Step() {
            double alpha = this.Alpha;
            this.Alpha += (0.0 - this.Alpha) * this._alphaDecay;

            this._active.Clear();
            foreach (ForceNode node in this.Nodes) {
                if (node.IsActive) {
                    this._active.Add(node);
                }
            }

            this.ApplyRepulsion(alpha);
            this.ApplyLinks(alpha);
            this.Integrate(alpha);
            // In der Abkühlphase wird die Überlappung gründlicher aufgelöst
            this.ResolveCollisions(alpha > 0.05 ? 3 : 14);
            this.KeepInside();
        }

        private void ApplyRepulsion(double alpha) {
            for (int i = 0; i < this._active.Count; i += 1) {
                ForceNode a = this._active[i];
                for (int j = i + 1; j < this._active.Count; j += 1) {
                    ForceNode b = this._active[j];
                    double dx = b.X - a.X;
                    double dy = b.Y - a.Y;
                    double distanceSquared = dx * dx + dy * dy;
                    if (distanceSquared < 1e-6) {
                        dx = this._random.NextDouble() - 0.5;
                        dy = this._random.NextDouble() - 0.5;
                        distanceSquared = dx * dx + dy * dy + 1e-6;
                    }
                    if (distanceSquared < MinDistanceSquared) {
                        distanceSquared = MinDistanceSquared;
                    }
                    double distance = Math.Sqrt(distanceSquared);
                    double force = Repulsion * a.Size * b.Size / distanceSquared * alpha;
                    double fx = dx / distance * force;
                    double fy = dy / distance * force;
                    a.Vx -= fx;
                    a.Vy -= fy;
                    b.Vx += fx;
                    b.Vy += fy;
                }
            }
        }

        private void ApplyLinks(double alpha) {
            foreach (ForceLink link in this.Links) {
                if (!link.A.IsActive || !link.B.IsActive) {
                    continue;
                }
                double dx = link.B.X - link.A.X;
                double dy = link.B.Y - link.A.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                if (distance < 1e-3) {
                    distance = 1e-3;
                }
                double idealDistance = LinkDistanceMin + (1.0 - link.Strength) * (LinkDistanceMax - LinkDistanceMin);
                double stiffness = LinkStrength * (0.25 + link.Strength);
                double force = (distance - idealDistance) / distance * stiffness * alpha;
                double fx = dx * force;
                double fy = dy * force;
                link.A.Vx += fx;
                link.A.Vy += fy;
                link.B.Vx -= fx;
                link.B.Vy -= fy;
            }
        }

        /// <summary>Zentrumskraft, Geschwindigkeitsbegrenzung, Reibung und Bewegung.</summary>
        private void Integrate(double alpha) {
            double centerX = this.Width / 2.0;
            double centerY = this.Height / 2.0;
            foreach (ForceNode node in this._active) {
                node.Vx += (centerX - node.X) * Gravity * alpha;
                node.Vy += (centerY - node.Y) * Gravity * alpha;
                double speed = Math.Sqrt(node.Vx * node.Vx + node.Vy * node.Vy);
                if (speed > MaxSpeed) {
                    node.Vx *= MaxSpeed / speed;
                    node.Vy *= MaxSpeed / speed;
                }
                if (node.IsPinned) {
                    node.Vx = 0;
                    node.Vy = 0;
                    continue;
                }
                node.Vx *= Friction;
                node.Vy *= Friction;
                node.X += node.Vx;
                node.Y += node.Vy;
            }
        }

        /// <summary>Schiebt überlappende Textfelder entlang der Achse mit der kleineren Überlappung auseinander.</summary>
        private void ResolveCollisions(int iterations) {
            for (int iteration = 0; iteration < iterations; iteration += 1) {
                for (int i = 0; i < this._active.Count; i += 1) {
                    ForceNode a = this._active[i];
                    for (int j = i + 1; j < this._active.Count; j += 1) {
                        ForceNode b = this._active[j];
                        double dx = b.X - a.X;
                        double dy = b.Y - a.Y;
                        double overlapX = (a.Width + b.Width) / 2.0 + Padding - Math.Abs(dx);
                        double overlapY = (a.Height + b.Height) / 2.0 + Padding - Math.Abs(dy);
                        if (overlapX <= 0 || overlapY <= 0) {
                            continue;
                        }
                        if (overlapX < overlapY) {
                            double shift = overlapX / 2.0 * (dx >= 0 ? 1 : -1);
                            if (!a.IsPinned) {
                                a.X -= shift * (b.IsPinned ? 2 : 1);
                            }
                            if (!b.IsPinned) {
                                b.X += shift * (a.IsPinned ? 2 : 1);
                            }
                        }
                        else {
                            double shift = overlapY / 2.0 * (dy >= 0 ? 1 : -1);
                            if (!a.IsPinned) {
                                a.Y -= shift * (b.IsPinned ? 2 : 1);
                            }
                            if (!b.IsPinned) {
                                b.Y += shift * (a.IsPinned ? 2 : 1);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Hält alle Textfelder innerhalb der Fläche.</summary>
        private void KeepInside() {
            foreach (ForceNode node in this._active) {
                node.X = Math.Min(Math.Max(node.X, node.Width / 2.0 + Margin), this.Width - node.Width / 2.0 - Margin);
                node.Y = Math.Min(Math.Max(node.Y, node.Height / 2.0 + Margin), this.Height - node.Height / 2.0 - Margin);
            }
        }
    }
}
