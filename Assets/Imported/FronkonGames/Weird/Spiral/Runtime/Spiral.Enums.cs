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

namespace FronkonGames.Weird.Spiral
{
  /// <summary> Shape type. </summary>
  public enum ShapeType
  {
    /// <summary> Circular droste effect. </summary>
    Circular = 0,

    /// <summary> Rectangular droste effect (4 sides). </summary>
    Rectangular = 1,

    /// <summary> Triangular droste effect (3 sides). </summary>
    Triangular = 2,

    /// <summary> Pentagonal droste effect (5 sides). </summary>
    Pentagonal = 3,

    /// <summary> Hexagonal droste effect (6 sides). </summary>
    Hexagonal = 4
  }

  /// <summary> Edge mode for UV handling. </summary>
  public enum EdgeMode
  {
    /// <summary> Clamp UVs at edges. </summary>
    Clamp = 0,

    /// <summary> Mirror UVs at edges. </summary>
    Mirror = 1,

    /// <summary> Repeat/tile UVs at edges. </summary>
    Repeat = 2
  }
}
