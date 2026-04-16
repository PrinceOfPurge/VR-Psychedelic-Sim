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

namespace FronkonGames.Weird.Edges
{
  public enum EdgeColorMode
  {
    // <summary> Linear edge coloring. </summary>
    Linear,

    // <summary> Depth-based edge coloring. </summary>
    DepthBased,
  }

  /// <summary> Edge detection method. </summary>
  public enum EdgeDetectionMethod
  {
    /// <summary> Depth-based edge detection using neighboring pixels. </summary>
    DepthNeighbors = 0,

    /// <summary> Classic Sobel operator for edge detection. </summary>
    Sobel = 1,

    /// <summary> Prewitt operator for edge detection. </summary>
    Prewitt = 2,

    /// <summary> Normal-based edge detection. </summary>
    NormalBased = 3,

    /// <summary> Color-based edge detection in RGB space. </summary>
    ColorBased = 4,

    /// <summary> Hybrid method combining depth and color detection. </summary>
    Hybrid = 5
  }
}
