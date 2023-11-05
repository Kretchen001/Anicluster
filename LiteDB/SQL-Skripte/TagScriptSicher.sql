---------------------------------------------------------------------------------------------------
--Löscht zunächst die gesamte Tags Tabelle
--
--		----------------------------------------------------------
--		| DELETE funktioniert wenn man ein END; dahinterschreibt |
--		----------------------------------------------------------
--
---------------------------------------------------------------------------------------------------
DROP COLLECTION Tags;

---------------------------------------------------------------------------------------------------
--Hinzufügen aller Datensätze
--		|-(Einträge vom Typ AnimeTag mit id als GUID und tagDesignator als string)
---------------------------------------------------------------------------------------------------

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000000"},
            tagDesignator: "Mecha"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000001"},
            tagDesignator: "Krieg"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000002"},
            tagDesignator: "Fantasy"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000003"},
            tagDesignator: "Battle-Royal"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000004"},
            tagDesignator: "Sci-Fiction"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000005"},
            tagDesignator: "Romanze"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000006"},
            tagDesignator: "Harem"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000007"},
            tagDesignator: "Apokalypse"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000008"},
            tagDesignator: "Post-Apokalypse"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000009"},
            tagDesignator: "Action"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000010"},
            tagDesignator: "Magie"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000011"},
            tagDesignator: "Komödie"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000012"},
            tagDesignator: "Schwertkampf"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000013"},
            tagDesignator: "Demonen"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000014"},
            tagDesignator: "Drachen"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000015"},
            tagDesignator: "Over-Powered (OP)"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000016"},
            tagDesignator: "Reincarnation"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000017"},
            tagDesignator: "Mysterik"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000018"},
            tagDesignator: "Abenteuer"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000019"},
            tagDesignator: "Drama"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000020"},
            tagDesignator: "Tragödie"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000021"},
            tagDesignator: "Isekai"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000022"},
            tagDesignator: "Monster"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000023"},
            tagDesignator: "Mittelalter"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000024"},
            tagDesignator: "Virtuelle Realität (VR)"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000025"},
            tagDesignator: "Dungeon"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000026"},
            tagDesignator: "Übermäßige Gewaltdarstellung"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000027"},
            tagDesignator: "Politik"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000028"},
            tagDesignator: "Royal"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000029"},
            tagDesignator: "Management"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000030"},
            tagDesignator: "Gott"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000031"},
            tagDesignator: "Religion"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000032"},
            tagDesignator: "Ecchi"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000033"},
            tagDesignator: "Zombie"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000034"},
            tagDesignator: "Slice-Of-Life"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000035"},
            tagDesignator: "Horror"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000036"},
            tagDesignator: "Androiden"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000037"},
            tagDesignator: "Krimi"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000038"},
            tagDesignator: "Katzen"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000039"},
            tagDesignator: "Hunde"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000040"},
            tagDesignator: "Elfen"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000041"},
            tagDesignator: "Medizin"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000042"},
            tagDesignator: "Militär"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000043"},
            tagDesignator: "Rivalität"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000044"},
            tagDesignator: "Gefängniss"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000045"},
            tagDesignator: "Geheime Identität"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000046"},
            tagDesignator: "Skelette"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000047"},
            tagDesignator: "Vampire"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000048"},
            tagDesignator: "Polizei"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000049"},
            tagDesignator: "Krankenhaus"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000050"},
            tagDesignator: "Feuerwehr"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000051"},
            tagDesignator: "Zeitreise"};
			
INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000052"},
            tagDesignator: "Reserve Harem"};
			
INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000053"},
            tagDesignator: "Sport"};
			
INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000054"},
            tagDesignator: "Demi"};
			
INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000055"},
            tagDesignator: "Heirat"};
			
INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000056"},
            tagDesignator: "Historie (Geschichte)"};
			
INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000057"},
            tagDesignator: "Parodie"};
			
INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000058"},
            tagDesignator: "Tiere"};
			
INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000059"},
            tagDesignator: "Engel"};
			
INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000060"},
            tagDesignator: "Crossover"};
			
INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000061"},
            tagDesignator: "Wirtschaft"};
			
INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000062"},
            tagDesignator: "Samurai"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000063"},
            tagDesignator: "Seinen"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000064"},
            tagDesignator: "Dark-Fantasy"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000065"},
            tagDesignator: "Psychothriller"};

INSERT INTO Tags
    VALUES {_id: {"$guid": "00000000-0000-0000-0000-000000000066"},
            tagDesignator: "Thriller"};
            