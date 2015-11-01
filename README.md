.NET Proxy (netProxy)
=====================

ASP.NET and JavaScript proxies for accessing external content.

The ASPX file can be used for returning external content over the current channel (HTTP/SSL). 

Used with the ASPX, the JS file can provide remote server access (no "same origin policy") with XMLHttpRequest syntax.

SSL Proxy
=========

Sometimes your web or intranet site is served over SSL but you'd like to include content from an insecure (HTTP-only) website. With this proxy, you can route requests to external sites through your own SSL certificate just by adding it to a query string.

**Example**

Requests for `http://example.com` would become requests for `netproxy.aspx?http://example.com`.

JavaScript Proxy
================

Another common goal is to incorporate content from third-party sites, but JavaScript has a "same origin policy" restriction meaning you can only access files on your own domain. With this proxy, you can GET content from or POST content to any server in the world using the familiar XMLHttpRequest syntax. All methods and properties are wrapped, meaning only the object declaration needs to be changed.

**Example**

`// Pass in the URL for our proxy ASPX page
var bingProxy = new netProxy("http://bing.com");
bingProxy.onreadystatechange = function(){
if (bingProxy.readyState == 4)
alert('Source code for http://bing.com:\r\n\r\n' +bingProxy.responseText);
};
bingProxy.open("GET", "http://bing.com", true);
bingProxy.send();`

History
=======

For previous releases and context, [view this project archive on CodePlex](https://netproxy.codeplex.com/).

License
=======

Copyright © 2011-2015 [Bert Johnson](https://bertjohnson.com)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
