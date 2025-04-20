using System.Reflection;

namespace Modells {

    public abstract class ValueComparableObject {

        private static readonly Dictionary<Type, MemberInfo[]> _equalityMemberCache = new();

        private static MemberInfo[] GetEqualityMembers(Type objectType) {
            if (_equalityMemberCache.TryGetValue(objectType, out MemberInfo[]? cachedMembers)) {
                return cachedMembers;
            }

            MemberInfo[] members = (
                from member in objectType.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                where (member is PropertyInfo property && property.CanRead && property.GetIndexParameters().Length == 0)
                      || member is FieldInfo
                select member
            ).ToArray();

            _equalityMemberCache[objectType] = members;
            return members;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj == null || obj.GetType() != this.GetType()) {
                return false;
            }

            MemberInfo[] membersToCompare = GetEqualityMembers(this.GetType());

            foreach (MemberInfo member in membersToCompare) {
                object? thisValue = GetMemberValue(member, this);
                object? otherValue = GetMemberValue(member, obj);

                if (!object.Equals(thisValue, otherValue)) {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode() {
            MemberInfo[] membersToHash = GetEqualityMembers(this.GetType());

            unchecked {
                int hash = 17;
                foreach (MemberInfo member in membersToHash) {
                    object? value = GetMemberValue(member, this);
                    hash = hash * 23 + (value?.GetHashCode() ?? 0);
                }

                return hash;
            }
        }

        private static object? GetMemberValue(MemberInfo member, object target) {
            return member switch {
                PropertyInfo property => property.GetValue(target),
                FieldInfo field => field.GetValue(target),
                _ => throw new NotSupportedException($"Unsupported member type: {member.MemberType}")
            };
        }
    }
}
