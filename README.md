# Glastroika

## Purpose
Glastroika is an unoffical Instagram API made in C# .NET with a seperate API and an example bot.

## Name Explaination
Back in the day i made an Instagram downloader with a generic name, but then Instagram changed the entire API, so suddently the program didn't work at all, so i just gave up.
But some time later i decided to try again and i wanted to give it a new name; so i named it "Perestroika" (Meaning "Restructuring") after the Russian word and the reformations in the USSR made by Mikhail Gorbachev, because you know, I restructured the code.

When Perestroika gave me problems, and because i didn't have the source code at hand; I decided to rewrite the entire thing in C# .NET Core, and to follow the naming pattern, i named it "Glasnost" (Meaning "Openness") after the other reformation made by Mikhail Gorbachev, though the name doens't make as much sense as Perestroika.

Because i wasn't really that impressed with .NET Core; i made a new project called Glastrioka which is a portmanteau of Perestroika and Glasnost... Clever right?

## Usage
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
