using System;
using System.Collections.Generic;

namespace MZPO.ucheba.ru.Models
{
    public class LeadLite
    {
#pragma warning disable IDE1006 // Naming Styles
        public int id { get; set; }
        public Person person { get; set; }
        public List<Activity> activities { get; set; }
        public Activity lastActivity { get; set; }

        public class Person
        {
            public int id { get; set; }
            public string fullName { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
        }

        public class Activity
        {
            public int id { get; set; }
            public ActivityType type { get; set; }
            public DateTime activityDoneAt { get; set; }
            public LearningRequest learningRequest { get; set; }
            public Question question { get; set; }
            public Program program { get; set; }

            public class ActivityType
            {
                public string value { get; set; }
                public string name { get; set; }
            }

            public class LearningRequest
            {
                public int id { get; set; }
                public Program program { get; set; }
            }

            public class Question
            {
                public int id { get; set; }
                public string text { get; set; }
                public Program program { get; set; }
            }
        }

        public class Program
        {
            public int id { get; set; }
            public string name { get; set; }
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}