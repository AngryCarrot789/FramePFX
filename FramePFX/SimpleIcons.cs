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

using FramePFX.Icons;
using SkiaSharp;

namespace FramePFX;

public static class SimpleIcons {
    public static readonly Icon AnIcon = IconManager.Instance.RegisterGeometryIcon("anIcon", SKColors.DarkGray, null, ["M0 11 0 0 16.8 0 16.8 11.6 10 11.6 12.6 13.8 8.7 13.8 4.8 10.3 8.7 6.6 12.6 6.6 10 9.2 14.4 9.2 14.4 2.6 2.6 2.6 2.6 9 4.6 9 3.6 10.3 4.5 11.6 0 11.6Z"]);
}