using System;

namespace MZPO.LeadProcessors
{
    public class EventProperties
    {
        public enum Type
        {
            styx,
            morizo,
            mkb,
            dod,
            openLesson,
            baseEvent
        }

#pragma warning disable IDE1006 // Naming Styles
        public int id { get; set; }
        public string page_name { get; set; }
        public DateTime vrema { get; set; }
        public string event_address { get; set; }
        public Type type { get
            {
                if (page_name.Contains("Международного клуба выпускников"))
                    return Type.mkb;

                if (page_name.Contains("STYX"))
                    return Type.styx;

                if (page_name.Contains("Morizo"))
                    return Type.morizo;

                if (page_name.Contains("День открытых дверей"))
                    return Type.dod;

                if (page_name.Contains("Пробный урок"))
                    return Type.openLesson;

                return Type.baseEvent;
            } }
#pragma warning restore IDE1006 // Naming Styles
    }
}