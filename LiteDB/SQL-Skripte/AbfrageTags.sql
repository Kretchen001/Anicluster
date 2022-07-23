---------------------------------------------------------------------------------------------------
--Fragt aus der Tabelle Tags
--		|- alle tagDesignator 's
--			|- diese AUFSTEIGEND (ASC) sortiert
--				|- in der Spalte erg
---------------------------------------------------------------------------------------------------
SELECT tagDesignator as erg
FROM Tags
ORDER BY tagDesignator ASC