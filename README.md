# HotTips
Visual Studio extension that provides Tip of the Day


How to contribute new tips:

Rough steps:
- fork the repo
- add tip in markdown file to Tips folder
- make new .md file Build Action property 'Content',
- make new .md file 'Copy to output Directory' property 'Copy Always',
- make new .md file 'Include in VSIX' property 'True',
- repeat above if you are adding an image.
- add tip entry to appropriate groups json. Create new json if new group.
- if creating a new group: update EmbeddedTipProvider.cs 
- uninstall Hot Tips from your main Visual Studio
- run Hot Tips extension project
- click next until you see your tip
- submit PR from your form into the justcla master
