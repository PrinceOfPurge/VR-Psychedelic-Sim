////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>. All rights reserved.
//
// THIS FILE CAN NOT BE HOSTED IN PUBLIC REPOSITORIES.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace FronkonGames.Weird.DitherFog
{
  public enum FogColorModes
  {
    /// <summary> Use a solid color. </summary>
    Solid,

    /// <summary> Use the average scene color. </summary>
    Auto,

    /// <summary> Use a gradient of colors. </summary>
    Gradient,
  }

  public enum DitheringModes
  {
    /// <summary> No dithering. </summary>
    None,

    /// <summary> Bayer 4x4 dithering. </summary>
    Bayer4x4,

    /// <summary> Bayer 8x8 dithering. </summary>
    Bayer8x8,

    /// <summary> Bayer 16x16 dithering. </summary>
    Bayer16x16,

    /// <summary> Bayer 32x32 dithering. </summary>
    Bayer32x32,
  }
}
