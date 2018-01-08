/**
 * Credit of PasswordAuthentication.Authenticate() goes to https://github.com/pandolia/qqbot.
 */

using System;
using System.Net;
using System.Threading;
using MoreLinq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using Cookie = System.Net.Cookie;

namespace DumbQQ.PasswordAuthentication
{
    public static class Password
    {
        public static ((string ptwebqq, string vfwebqq, ulong uin, string psessionid) tokens, CookieContainer cookies)
            AuthenticateUsingPhantomJs(ulong username, string password)
        {
            return Authenticate(username, password, new PhantomJSDriver(), TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(1));
        }

        public static ((string ptwebqq, string vfwebqq, ulong uin, string psessionid) tokens, CookieContainer cookies)
            AuthenticateUsingFirefox(ulong username, string password)
        {
            return Authenticate(username, password, new FirefoxDriver(), TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(1));
        }

        public static ((string ptwebqq, string vfwebqq, ulong uin, string psessionid) tokens, CookieContainer cookies)
            AuthenticateUsingChronme(ulong username, string password)
        {
            return Authenticate(username, password, new ChromeDriver(), TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(1));
        }

        public static ((string ptwebqq, string vfwebqq, ulong uin, string psessionid) tokens, CookieContainer cookies)
            Authenticate(ulong username, string password, RemoteWebDriver driver, TimeSpan timeout,
                TimeSpan pausePeriod)
        {
            using (driver)
            {
                var wait = new WebDriverWait(driver, timeout);
                driver.Navigate().GoToUrl(@"https://m.qzone.com");
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.Id(@"go")));
                driver.FindElementById(@"u").SendKeys(username.ToString());
                driver.FindElementById(@"p").SendKeys(password);
                driver.FindElementById(@"go").Click();
                wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(@"header")));
                driver.Navigate().GoToUrl(@"https://w.qq.com");
                try
                {
                    driver.SwitchTo().Frame(@"ptlogin");
                    Thread.Sleep(pausePeriod);
                    wait.Until(ExpectedConditions.ElementToBeClickable(By.ClassName(@"face"))).Click();
                }
                catch
                {
                    // ignored
                }

                driver.Navigate().GoToUrl(@"https://web2.qq.com");

                var cookies = new CookieContainer();
                driver.Manage().Cookies.AllCookies.ForEach(x =>
                    cookies.Add(
                        new Cookie(x.Name, x.Value, x.Path, x.Domain)));

                driver.Navigate().GoToUrl(@"https://w.qq.com");
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.ClassName(@"container")));
                driver.Navigate().GoToUrl(@"https://web2.qq.com");
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.ClassName(@"container")));
                driver.Navigate().GoToUrl(@"https://w.qq.com");
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.ClassName(@"container")));
                Thread.Sleep(pausePeriod);

                var ptwebqq = (string) driver.ExecuteScript(@"return mq.ptwebqq");
                var vfwebqq = (string) driver.ExecuteScript(@"return mq.vfwebqq");
                var psessionid = (string) driver.ExecuteScript(@"return mq.psessionid");

                return ((ptwebqq, vfwebqq, username, psessionid), cookies);
            }
        }
    }
}