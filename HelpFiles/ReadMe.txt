To simplify creating the text for HTML help files HelpFiles.exe converts
plain text files, automatically inserting <br> wherever there is an 
end of line.  It also inserts the HTML for the menu before the text.

After editing one of the .txt files (or the menu files) run HelpFiles.exe
which creates Help HTML files from
 * the menu bar menu.ht
 * the HTML pages from text
 * the wrap up menu.ml
and outputs the results into /public from where they will be published
in a GitLab page (https://mr_belowski.gitlab.io/CrewChiefV4/index.html)
as well as being used by the Crew Chief "Help"

For now changes made in the HTMLhelp branch go to the page. .gitlab-ci.yml will
have to be edited to make it work from the main branch.
