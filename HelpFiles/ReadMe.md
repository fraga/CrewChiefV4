To simplify creating the content for HTML help files HelpFiles.exe inserts each content HTML file into menu.HTML.

After editing one of the content files (or the menu file) run HelpFiles.exe which creates Help HTML files and outputs the results into /public from where they will be published in a GitLab page 
(https://mr_belowski.gitlab.io/CrewChiefV4/index.html) by the GitLab CI/CD on the main branch (see .gitlab-ci.yml)

**To add a Page**
- Edit menu.html (I recommend using a text editor and just cut and paste an existing entry)
- Add the page to the list ```pageNames``` in HelpFiles/program.cs

**To add an Image**
- Add the image to the list ```images``` in HelpFiles/program.cs

**To add a Game**
- Add the game to the list ```gameNames``` in HelpFiles/program.cs
- Add a page *GettingStarted_GameSpecific_[gameName]*
- Add a page *GameSpecific_ForEachGame_[gameName]*
