// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using PFXToolKitUI.Icons;
using PFXToolKitUI.Themes;

namespace FramePFX;

public static class SimpleIcons {
    // Can't remember where I got this from... I'm pretty sure it was public domain but I try to credit anyway
    public static readonly Icon LoopIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(LoopIcon),
            BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static"),
            null,
            ["M0 11 0 0 16.8 0 16.8 11.6 10 11.6 12.6 13.8 8.7 13.8 4.8 10.3 8.7 6.6 12.6 6.6 10 9.2 14.4 9.2 14.4 2.6 2.6 2.6 2.6 9 4.6 9 3.6 10.3 4.5 11.6 0 11.6Z"]);

    public static readonly Icon ResetIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(ResetIcon),
            BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static"),
            null,
            ["F1 M 38,20.5833C 42.9908,20.5833 47.4912,22.6825 50.6667,26.046L 50.6667,17.4167L 55.4166,22.1667L 55.4167,34.8333L 42.75,34.8333L 38,30.0833L 46.8512,30.0833C 44.6768,27.6539 41.517,26.125 38,26.125C 31.9785,26.125 27.0037,30.6068 26.2296,36.4167L 20.6543,36.4167C 21.4543,27.5397 28.9148,20.5833 38,20.5833 Z M 38,49.875C 44.0215,49.875 48.9963,45.3932 49.7703,39.5833L 55.3457,39.5833C 54.5457,48.4603 47.0852,55.4167 38,55.4167C 33.0092,55.4167 28.5088,53.3175 25.3333,49.954L 25.3333,58.5833L 20.5833,53.8333L 20.5833,41.1667L 33.25,41.1667L 38,45.9167L 29.1487,45.9167C 31.3231,48.3461 34.483,49.875 38,49.875 Z"], stretch: StretchMode.Uniform);

    // https://www.svgrepo.com/svg/486816/rename
    public static readonly Icon RenameIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(RenameIcon),
            BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static"),
            null,
            ["M34.5745 27.5184c-2.1173.315-3.9546 1.6275-4.9393 3.5279v7.7617c.9302 2.1029 2.993 3.4782 5.292 3.5279 2.8225 0 4.5863-2.8225 4.5863-7.4088 0-4.5863-2.1168-7.4088-4.9391-7.4088ZM16.2288 35.2799H14.112c-4.2336.3529-5.9976 1.7641-5.9976 4.2336-.064.8601.2499 1.7055.8598 2.3154.6099.6099 1.4552.9238 2.3154.8598 2.0786-.1513 3.9386-1.347 4.9392-3.1752V35.2799Zm3.5281-8.4672c1.2449 2.1285 1.7417 4.6125 1.4111 7.0561v6.3504c-.0698 1.8886.0483 3.7795.3527 5.6447H16.9344V43.3944c-1.6836 1.9185-4.1543 2.9589-6.7032 2.8225-1.8052.2133-3.6148-.3615-4.966-1.5775-1.3511-1.216-2.1128-2.9553-2.09-4.7729-.2562-2.5544 1.171-4.9806 3.528-5.9977 2.013-.8161 4.1815-1.1776 6.3504-1.0584h3.5281v-.3527c.1469-1.3498-.3261-2.6933-1.2861-3.6533-.96-.96-2.3035-1.433-3.6532-1.286-2.0962.1856-4.1347.7852-5.9976 1.7641L4.2336 25.7543c2.516-1.2537 5.305-1.86 8.1144-1.7639 2.7726-.2514 5.5062.79 7.4088 2.8223Zm9.8783-11.9951V26.8127c1.5064-1.9901 3.8544-3.1641 6.3504-3.1752 5.292 0 8.8199 4.5865 8.8199 11.2897-.1821 3.1855-1.2851 6.2495-3.1752 8.8199-1.4571 1.8152-3.6702 2.8565-5.9975 2.8225-2.6061.0878-5.0482-1.2689-6.3504-3.5279v3.1752H24.3432c0-.789.0361-1.4699.085-2.1462l.0131-.1765.0136-.1766c.1034-1.3265.2411-2.7045.2411-4.9095V14.8176h4.9393ZM45.8639 10.584H0V52.92H45.8639V10.584Zm24.6961 0H56.4479v7.0561H63.504V45.8639H56.4479V52.92H70.5601V10.584ZM59.9761 0H42.336V3.528l7.0559-0V59.9759l-7.0559.0002V63.504l7.0559-.0002.0002.0002H52.92v-.0002l7.0561.0002V59.9761L52.92 59.9759V3.5279l7.0561 0V0Z"], stretch: StretchMode.Uniform);

    // https://www.svgrepo.com/svg/522461/video
    public static readonly Icon VideoIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(VideoIcon),
            null,
            BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static"),
            [
                "M20 11.5H3V20.5C3 21.0523 3.44772 21.5 4 21.5H20C20.5523 21.5 21 21.0523 21 20.5V12.5C21 11.9477 20.5523 11.5 20 11.5Z",
                "M1.59998 7.40002L17.5747 1.58568C18.0937 1.39679 18.6676 1.66438 18.8565 2.18335L19.5405 4.06274C19.7294 4.58172 19.4618 5.15556 18.9428 5.34445L2.96806 11.1588L1.59998 7.40002Z",
                "M15.6954 2.26973L15.1841 6.71254",
                "M11.9366 3.6378L11.4253 8.08061",
                "M8.17785 5.00589L7.66654 9.4487",
                "M4.41906 6.37397L3.90775 10.8168",
            ], 1.0, stretch: StretchMode.Uniform);

    // https://www.svgrepo.com/svg/522461/video
    public static readonly Icon BinIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(BinIcon),
            BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static"),
            null,
            ["M5.586 12.8h1.596V6.4H5.586v6.4Zm3.192 0h1.596V6.4h-1.596v6.4Zm-4.788 1.6h7.98V4.8H3.99V14.4ZM5.586 3.2h4.788V1.6H5.586V3.2Zm6.384 0V0H3.99V3.2H0V4.8H2.394V16h11.172V4.8H15.96V3.2H11.97Z",], stretch: StretchMode.Uniform);

    public static readonly Icon PlayPauseIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(PlayPauseIcon),
            BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static"),
            null,
            ["M0 10 0 0 8 5 0 10M9 10V0H12V10H9M14 0H17V10H14V0"], stretch: StretchMode.None);

    public static readonly Icon PlayIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(PlayIcon),
            BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static"),
            null,
            ["M0 15 0 0 15 7.5 0 15"], stretch: StretchMode.None);

    public static readonly Icon PauseIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(PauseIcon),
            BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static"),
            null,
            ["M0 15V0H5V15H0M10 0H15V15H10V0"], stretch: StretchMode.None);

    public static readonly Icon StopIcon =
        IconManager.Instance.RegisterGeometryIcon(
            nameof(StopIcon),
            BrushManager.Instance.GetDynamicThemeBrush("ABrush.Glyph.Static"),
            null,
            ["M0 13V0H13V13H0"], stretch: StretchMode.None);
}