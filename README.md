# Glastroika

## Purpose
Glastroika is an unoffical Instagram API made in C# .NET with an example bot.

## Name Explaination
Back in the day i made an Instagram downloader with a generic name, but then Instagram changed the entire API, so suddently the program didn't work at all, so i just gave up.
But some time later i decided to try again and i wanted to give it a new name; so i named it "Perestroika" (Meaning "Restructuring") after the Russian word and the reformations in the USSR made by Mikhail Gorbachev, because you know, I restructured the code.

When Perestroika gave me problems, and because i didn't have the source code at hand; I decided to rewrite the entire thing in C# .NET Core, and to follow the naming pattern, i named it "Glasnost" (Meaning "Openness") after the other reformation made by Mikhail Gorbachev, though the name doesn't make as much sense as Perestroika.

Because i wasn't really that impressed with .NET Core; i made a new project called Glastroika which is a portmanteau of Perestroika and Glasnost... Clever right?

## Usage
### Get User Data
```csharp
// Request User data from Instagram
User user = Instagram.GetUser("arianagrande");

Console.WriteLine(user.FullName);
Console.WriteLine(user.Biography);

// Enumerate through the users content and display their URL.
foreach(Media media in user.Media)
{
     foreach (string url in media.URL)
     {
          Console.WriteLine(url);
     }
}
```
### Get Media Data
GetMedia can currently get:
- The media ID
- The type (Image, Video, Collage)
- The caption
- The timestamp (In Unix time)
- The URLs of the media
- Likes
- Comments

```csharp
// Request Media data from Instagram
Media media = Instagram.GetMedia("BryOCFOA20l");

Console.WriteLine(media.Caption);
Console.WriteLine(media.Type.ToString());
Console.WriteLine(media.Timestamp);

foreach(string url in media.URL)
{
     Console.WriteLine(url);
}
```
