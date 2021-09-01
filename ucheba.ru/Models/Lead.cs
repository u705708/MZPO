using System;
using System.Collections.Generic;

namespace MZPO.ucheba.ru.Models
{
    public class Lead
    {
#pragma warning disable IDE1006 // Naming Styles
        public int id { get; set; }
        public Person person { get; set; }
        public SourceType sourceType { get; set; }
        public LastActivity lastActivity { get; set; }
        public List<Object> programsOfInterest { get; set; }

        public class Person
        {
            public int id { get; set; }
            public string fullName { get; set; }
            public Location location { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string yearOfBirth { get; set; }
            public string gender { get; set; }
            public LearningInterest learningInterest { get; set; }
            public string schoolName { get; set; }
            public int? schoolGrade { get; set; }
            public Object avatar { get; set; }
            public Object avatarThumbnail { get; set; }
            public List<Object> socialAccounts { get; set; }
            public List<EnrolleeInfo> enrolleeInfo { get; set; }

            public class Location
            {
                public int id { get; set; }
                public string name { get; set; }
            }

            public class LearningInterest
            {
                public int id { get; set; }
                public bool isAbroadEducation { get; set; }
                public List<ProgramType> programTypes { get; set; }
                public List<Object> trainingFormTypes { get; set; }
                public List<Location> locations { get; set; }
                public List<Tag> tags { get; set; }

                public class ProgramType
                { 
                    public int value { get; set; }
                    public string name { get; set; }
                }

                public class Tag
                {
                    public int id { get; set; }
                    public string name { get; set; }
                    public List<Object> children { get; set; }
                }
            }

            public class EnrolleeInfo
            {
                public int id { get; set; }
                public string year { get; set; }
                public string educationLevel { get; set; }
                public List<Object> olympiads { get; set; }
                public List<Object> exams { get; set; }
                public string schoolAttestat { get; set; }
            }
        }

        public class SourceType
        {
            public int id { get; set; }
            public int name { get; set; }
        }

        public class LastActivity
        {
            public int id { get; set; }
            public ActivityType type { get; set; }
            public DateTime activityDoneAt { get; set; }
            public LearningRequest learningRequest { get; set; }
            public string question { get; set; }
            public string result { get; set; }
            public string program { get; set; }
            public string faculty { get; set; }
            public string url { get; set; }
            public string name { get; set; }
            public string conference { get; set; }

            public class ActivityType
            {
                public string value { get; set; }
                public string name { get; set; }
            }

            public class LearningRequest
            {
                public int id { get; set; }
                public string program { get; set; }
                public string faculty { get; set; }
                public ProgramType programType { get; set; }

                public class ProgramType
                {
                    public int value { get; set; }
                    public string name { get; set; }
                }
            }
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}