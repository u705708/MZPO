namespace MZPO.Services
{
    public class LeadsSorter
    {
        private bool callMarker;
        private bool leadMarker;

        public LeadsSorter()
        {
            callMarker = true;
            leadMarker = true;
        }

        public int GetCallChoice()
        {
            callMarker = !callMarker;
            return callMarker ? 1 : 0;
        }

        public bool GetLeadChoice()
        {
            leadMarker = !leadMarker;
            return leadMarker;
        }
    }
}