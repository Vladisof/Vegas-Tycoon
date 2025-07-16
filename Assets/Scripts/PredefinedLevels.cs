using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class PredefinedLevels : MonoBehaviour
    {

        public List<LevelData> GetPredefinedLevels()
{
    return new List<LevelData>
    {
        new LevelData
        {
            levelName = "Solo Debut",
            requiredStars = 1,
            requiredMoney = 800,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Singer", count = 1 }
            },
            recommendedTags = new List<string>{ "Charming" },
            restrictions = new List<LevelRestriction>()
        },
        new LevelData
        {
            levelName = "Dance Off",
            requiredStars = 1,
            requiredMoney = 1300,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Singer", count = 1 }
            },
            recommendedTags = new List<string>{ "Loud", "Energetic" },
            restrictions = new List<LevelRestriction>()
        },
        new LevelData
        {
            levelName = "Magic Moment",
            requiredStars = 2,
            requiredMoney = 1600,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Magician", count = 1 },
                new LevelRoleRequirement{ role = "Host", count = 1 }
            },
            recommendedTags = new List<string>{ "Weird", "Showy" },
            restrictions = new List<LevelRestriction>()
        },
        new LevelData
        {
            levelName = "Tech & Dance",
            requiredStars = 2,
            requiredMoney = 1900,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Tech", count = 1 },
                new LevelRoleRequirement{ role = "Dancer", count = 1 }
            },
            recommendedTags = new List<string>{ "Futuristic", "Edgy" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ forbiddenRole = "Singer" }
            }
        },
        new LevelData
        {
            levelName = "Classical Night",
            requiredStars = 2,
            requiredMoney = 2100,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Singer", count = 1 },
                new LevelRoleRequirement{ role = "Acrobat", count = 1 }
            },
            recommendedTags = new List<string>{ "Classical" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ mustIncludeTag = "Classical" }
            }
        },
        new LevelData
        {
            levelName = "Futuristic Frenzy",
            requiredStars = 2,
            requiredMoney = 2300,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Tech", count = 1 },
                new LevelRoleRequirement{ role = "Magician", count = 1 }
            },
            recommendedTags = new List<string>{ "Futuristic", "Stylish" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ mustIncludeTag = "Futuristic" }
            }
        },
        new LevelData
        {
            levelName = "Host's Challenge",
            requiredStars = 2,
            requiredMoney = 2450,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Host", count = 1 },
                new LevelRoleRequirement{ role = "Singer", count = 1 }
            },
            recommendedTags = new List<string>{ "Showy" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ forbiddenRole = "Tech" }
            }
        },
        new LevelData
        {
            levelName = "Energetic Mix",
            requiredStars = 2,
            requiredMoney = 2600,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Singer", count = 1 },
                new LevelRoleRequirement{ role = "Magician", count = 1 }
            },
            recommendedTags = new List<string>{ "Energetic", "Weird" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ forbiddenRole = "Acrobat" }
            }
        },
        new LevelData
        {
            levelName = "Dramatic Showdown",
            requiredStars = 2,
            requiredMoney = 2750,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Singer", count = 1 },
                new LevelRoleRequirement{ role = "Host", count = 1 },
                new LevelRoleRequirement{ role = "Tech", count = 1 }
            },
            recommendedTags = new List<string>{ "Dramatic", "Charming" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ mustIncludeTag = "Dramatic" }
            }
        },
        new LevelData
        {
            levelName = "Edgy Ensemble",
            requiredStars = 2,
            requiredMoney = 2950,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Tech", count = 1 },
                new LevelRoleRequirement{ role = "Magician", count = 1 },
                new LevelRoleRequirement{ role = "Dancer", count = 1 }
            },
            recommendedTags = new List<string>{ "Edgy", "Loud" },
            restrictions = new List<LevelRestriction>()
        },
        new LevelData
        {
            levelName = "Stylish Affair",
            requiredStars = 2,
            requiredMoney = 3150,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Host", count = 1 },
                new LevelRoleRequirement{ role = "Acrobat", count = 1 },
                new LevelRoleRequirement{ role = "Singer", count = 1 }
            },
            recommendedTags = new List<string>{ "Stylish", "Showy" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ forbiddenRole = "Tech" }
            }
        },
        new LevelData
        {
            levelName = "Animal Parade",
            requiredStars = 2,
            requiredMoney = 3350,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Acrobat", count = 1 }
            },
            recommendedTags = new List<string>{ "Animal Lover" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ mustIncludeTag = "Animal Lover" }
            }
        },
        new LevelData
        {
            levelName = "Quiet Night",
            requiredStars = 2,
            requiredMoney = 3550,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Magician", count = 1 },
                new LevelRoleRequirement{ role = "Singer", count = 1 }
            },
            recommendedTags = new List<string>{ "Quiet", "Edgy" },
            restrictions = new List<LevelRestriction>()
        },
        new LevelData
        {
            levelName = "Weird Wonders",
            requiredStars = 2,
            requiredMoney = 3750,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Magician", count = 1 },
                new LevelRoleRequirement{ role = "Host", count = 1 }
            },
            recommendedTags = new List<string>{ "Weird", "Showy" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ forbiddenRole = "Tech" }
            }
        },
        new LevelData
        {
            levelName = "Loud Crowd",
            requiredStars = 2,
            requiredMoney = 3950,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Singer", count = 1 },
                new LevelRoleRequirement{ role = "Acrobat", count = 1 }
            },
            recommendedTags = new List<string>{ "Loud", "Classical" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ mustIncludeTag = "Loud" }
            }
        },
        new LevelData
        {
            levelName = "Showy Stunt",
            requiredStars = 2,
            requiredMoney = 4100,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Host", count = 1 }
            },
            recommendedTags = new List<string>{ "Showy", "Charming" },
            restrictions = new List<LevelRestriction>()
        },
        new LevelData
        {
            levelName = "Acrobatic Artistry",
            requiredStars = 2,
            requiredMoney = 4250,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Acrobat", count = 2 }
            },
            recommendedTags = new List<string>{ "Classical", "Stylish" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ forbiddenRole = "Magician" }
            }
        },
        new LevelData
        {
            levelName = "Final Countdown",
            requiredStars = 3,
            requiredMoney = 4400,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Singer", count = 1 },
                new LevelRoleRequirement{ role = "Tech", count = 1 },
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Host", count = 1 }
            },
            recommendedTags = new List<string>{ "Futuristic", "Weird" },
            restrictions = new List<LevelRestriction>()
        },
        new LevelData
        {
            levelName = "Gala Premiere",
            requiredStars = 3,
            requiredMoney = 4800,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Acrobat", count = 1 },
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Magician", count = 1 },
                new LevelRoleRequirement{ role = "Host", count = 1 }
            },
            recommendedTags = new List<string>{ "Stylish", "Edgy" },
            restrictions = new List<LevelRestriction>
            {
                new LevelRestriction{ mustIncludeTag = "Stylish" }
            }
        },
        new LevelData
        {
            levelName = "Vegas Finale",
            requiredStars = 3,
            requiredMoney = 5600,
            requiredRoles = new List<LevelRoleRequirement>
            {
                new LevelRoleRequirement{ role = "Singer", count = 1 },
                new LevelRoleRequirement{ role = "Dancer", count = 1 },
                new LevelRoleRequirement{ role = "Acrobat", count = 1 },
                new LevelRoleRequirement{ role = "Tech", count = 1 },
                new LevelRoleRequirement{ role = "Host", count = 1 },
                new LevelRoleRequirement{ role = "Magician", count = 1 }
            },
            recommendedTags = new List<string>{ "Classical", "Futuristic", "Weird" },
            restrictions = new List<LevelRestriction>()
        }
    };
}

    }
}