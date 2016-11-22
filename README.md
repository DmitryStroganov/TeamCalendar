# TeamCalendar
HTML-based viewer of co-workers and meeting room occupancy information.

* This is way to display resource daily calendar information from your MS Exchange as plain web page (i.e. html grid).
* This is a must-have for agile processes, e.g. sprint planning.
* Most typically it is used as Wall TV in the office.

![team calendar - free preview ](/docs/TeamCalendar_free_preview.png)

# How it works / Technical design

![team calendar - architecture diagram ](/docs/Architecture_diagram.png)

Team Calendar connects to your corporate MS Exchange Server or MS Office 365, using a read-only account and displays live, aggregated calendar view in a web browser.

* Platform / technology: MS ASP .Net 4.5
* Web server: MS IIS6+
* Client: a web browser with HTML, Java Script and XHR support (IE6+)

# Free / opensource version

This open repository contains full implementation of the actual Team Calendar, but does not include some of the advanced / pro features.
The calendar implementation is easily extendible, by a programmer, using provided interfaces / api's.

# Commercial version

Commercial version includes support, and the following features:
* MS Exchange calendar data provider 
* Crypto provider implementation, for obfuscating sensitive contents of web.config.

MS Exchange calendar data provider or full package of the product is available via this page:
http://dmitrystroganov.dk/projects/TeamCalendar/
