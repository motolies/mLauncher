using System.ComponentModel;

namespace mRenamer.Model
{
    public enum PositionEnum
    {
        [Description("앞에 붙이기")]
        PreFix = 1 << 0,
        [Description("뒤에 붙이기")]
        PostFix = 1 << 1
    }
}
