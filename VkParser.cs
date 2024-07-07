using Eq.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Eq;

public class VkParser
{
    public async Task<List<TrackModel>> ParseVkPlaylist(string url)
    {
        var options = new ChromeOptions();
        
        options.AddArgument("--headless");
        using var driver = new ChromeDriver(options);
        driver.Navigate().GoToUrl(url);
        
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        wait.Until(d => d.FindElements(By.ClassName("AudioPlaylistSnippet__actionButton")).Count > 0);
        
        var button = driver.FindElement(By.ClassName("AudioPlaylistSnippet__actionButton"));
        button.Click();
        
        await Task.Delay(4000);
    
        var songElements = driver.FindElements(By.ClassName("audio_row__title_inner"));
        var artistElements = driver.FindElements(By.ClassName("audio_row__performers"));

        var tracks = songElements.Zip(artistElements, (song, artist) => 
            new TrackModel { Name = song.Text, Artist = artist.Text }).ToList();

        for (var index = 0; index < tracks.Count; index++)
        {
            var t = tracks[index];
            Console.WriteLine($"{index} | Artist: {t.Artist}, Song: {t.Name}");
        }

        return tracks;
    }
}

// using var driver = new ChromeDriver();
//
// driver.Navigate().GoToUrl(url);
//
// Thread.Sleep(5000);
//         
// var js = (IJavaScriptExecutor)driver;
//
// for (var i = 0; i < 10; i++)
// {
//     js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
//     Thread.Sleep(1000);
// }
//         
// var trackElements = driver.FindElements(By.XPath("//span[@class='ai_title']"));
//
// var tracks = trackElements.Select(x => x.GetAttribute("innerHTML")).ToList();
//         
// return tracks;