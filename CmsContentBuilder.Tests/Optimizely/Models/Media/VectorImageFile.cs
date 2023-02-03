using EPiServer.DataAnnotations;
using EPiServer.Framework.Blobs;
using EPiServer.Framework.DataAnnotations;
using Optimizely.Demo.PublicWeb.Models.Media.Base;

namespace Optimizely.Demo.PublicWeb.Models.Media;

[ContentType(GUID = "{42A72199-0846-4405-B3BE-A929B7DCDE97}")]
[MediaDescriptor(ExtensionString = "svg")]
public class VectorImageFile : ImageFileBase
{
    public override Blob Thumbnail => BinaryData;
}
