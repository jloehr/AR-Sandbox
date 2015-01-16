# .Net Image Library

.NET library for easy image handling like:
  * Filters
  * Cropping
  * Thumbnail creation
  * Saving as JPG/GIF/PNG files or streams
  * Opening images from streams or URLs

You'll find information on how to get started at http://kaliko.com/image-library/get-started/ and full API documentation at http://kaliko.com/image-library/api/

<a href="https://docs.google.com/uc?export=download&id=0ByDd6qehepdVNXFJZm9MYW13dDQ">Download latest binary version, 2.0.0</a>

Using only safe code making this library possible to use on web hosts with medium trust.

Current build contains the following filters:
  * Gaussian blur filter
  * Unsharpen mask filter
  * Chroma key filter
  * Contrast filter
  * Brightness filter
  * Invert filter
  * Desaturnation filter

If you plan using this library with WPF or simular, read this post on <a href="http://labs.kaliko.com/2011/03/convert-to-bitmapimage.html">how to convert an KalikoImage object to System.Windows.Media.Imaging.BitmapImage and System.Windows.Controls.Image</a>.

## History
**2.0.0**
  * Replaced Gaussian blur filter with better implementation (affects unsharpen masks)
  * Added chroma key filter
  * Rewritten API for Scaling
  * Added color space handling

**1.2.4**
  * Updated to Visual Studio 2010.
  * Code clean-up.<br>
  * Unwanted-border-artifact-problem fixed (thanks Richard!)<br>
  * IDisponable has been implemented.<br>

**1.2.3**
  * Minor changes.
  * First API documentation uploaded. Still missing a whole lot, but it's a start :)

**1.2.2**
  * Minor changes

**1.2.1**
  * Bug in thumbnail function fixed.
  * Code cleaned up.
