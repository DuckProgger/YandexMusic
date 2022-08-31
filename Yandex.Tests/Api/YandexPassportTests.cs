using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using Yandex.Api.Passport;
using Yandex.Tests.Internal;

namespace Yandex.Tests.Api;

[TestClass]
public class YandexPassportTests
{
    [TestMethod]
    public async Task Authorization() {
        PassportWebAuthData token = await TestFactory.GetPassportApi()
            .WebAuthAsync(TestFactory.Configuration.Login, TestFactory.Configuration.Password, default);

        Assert.IsNotNull(token.SessionId);
        Assert.IsNotNull(token.YandexUid);
    }

    [TestMethod]
    public async Task MobileAuthorization() {
        PassportMobileAuthData token = await TestFactory.GetPassportApi()
            .MobileAuthAsync(TestFactory.Configuration.Login, TestFactory.Configuration.Password, default);

        Assert.IsNotNull(token.AccessToken);
    }
}
