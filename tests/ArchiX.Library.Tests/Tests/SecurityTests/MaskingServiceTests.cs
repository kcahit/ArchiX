#nullable enable
using ArchiX.Library.Services.Security;
using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests
{
 public sealed class MaskingServiceTests
 {
 [Fact]
 public void Mask_Basic()
 {
 var svc = new MaskingService();
 Assert.Equal("ab****yz", svc.Mask("ab1234yz",2,2, '*'));
 }

 [Fact]
 public void MaskEmail_Basic()
 {
 var svc = new MaskingService();
 Assert.Equal("t**t@example.com", svc.MaskEmail("test@example.com"));
 }
 }
}
