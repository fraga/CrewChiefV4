using ksBroadcastingNetwork;
using ksBroadcastingNetwork.Structs;
using System;
using System.Linq;

namespace ksBroadcastingTestClient.Broadcasting
{
    public class LapViewModel : KSObservableObject
    {
        public int? LaptimeMS { get => Get<int?>(); private set => Set(value); }
        public string LaptimeString { get => Get<string>(); private set => Set(value); }
        public int? Split1MS { get => Get<int?>(); private set => Set(value); }
        public string Split1String { get => Get<string>(); private set => Set(value); }
        public int? Split2MS { get => Get<int?>(); private set => Set(value); }
        public string Split2String { get => Get<string>(); private set => Set(value); }
        public int? Split3MS { get => Get<int?>(); private set => Set(value); }
        public string Split3String { get => Get<string>(); private set => Set(value); }

        public LapType Type { get => Get<LapType>(); private set => Set(value); }
        public bool IsValid { get => Get<bool>(); private set => Set(value); }
        public string LapHint { get => Get<string>(); private set => Set(value); }

        internal void Update(LapInfo lapUpdate)
        {
            var isChanged = LaptimeMS != lapUpdate.LaptimeMS;
            if (isChanged)
            {
                LaptimeMS = lapUpdate.LaptimeMS;
                if (LaptimeMS == null)
                    LaptimeString = "--";
                else
                    LaptimeString = $"{TimeSpan.FromMilliseconds(LaptimeMS.Value):mm\\:ss\\.fff}";

                Split1MS = lapUpdate.Splits.FirstOrDefault();
                if (Split1MS != null)
                    Split1String = $"{TimeSpan.FromMilliseconds(Split1MS.Value):ss\\.f}";
                else
                    Split1String = "";

                Split2MS = lapUpdate.Splits.Skip(1).FirstOrDefault();
                if (Split2MS != null)
                    Split2String = $"{TimeSpan.FromMilliseconds(Split2MS.Value):ss\\.f}";
                else
                    Split2String = "";

                Split3MS = lapUpdate.Splits.Skip(2).FirstOrDefault();
                if (Split3MS != null)
                    Split3String = $"{TimeSpan.FromMilliseconds(Split3MS.Value):ss\\.f}";
                else
                    Split3String = "";

                Type = lapUpdate.Type;
                IsValid = lapUpdate.IsValidForBest;

                if (Type == LapType.Outlap)
                    LapHint = "OUT";
                else if (Type == LapType.Inlap)
                    LapHint = "IN";
                else
                    LapHint = "";
            }
        }
    }
}
