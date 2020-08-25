To simplify creating the content for HTML help files HelpFiles.exe
prepends the menu and appends the footer from menu.HTML to each
content HTML file.

After editing one of the content files (or the menu file) run HelpFiles.exe
which creates Help HTML files and outputs the results into /public 
from where they will be published in a GitLab page 
(https://mr_belowski.gitlab.io/CrewChiefV4/index.html)
as well as being used by the Crew Chief "Help"

For now changes made in the HTMLhelp branch go to the page. .gitlab-ci.yml will
have to be edited to make it work from the main branch.
