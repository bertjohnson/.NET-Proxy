// .NET Proxy (netProxy) by Bert Johnson <http://bertjohnson.net>
// http://netproxy.codeplex.com

function netProxy(baseURL){
    this.baseURL = baseURL;

    this.getXMLHttpRequest = function(){
	    if (typeof(XMLHttpRequest) == "undefined"){
		    try { return new ActiveXObject("MSXML3.XMLHTTP") }catch(e){}
		    try { return new ActiveXObject("MSXML2.XMLHTTP.3.0") }catch(e){}
		    try { return new ActiveXObject("Msxml2.XMLHTTP") }catch(e){}
		    try { return new ActiveXObject("Microsoft.XMLHTTP") }catch(e){}
		    return null;
	    }
	    return new XMLHttpRequest();
    };

    this.XMLHttpRequest = this.getXMLHttpRequest();
    this.XMLHttpRequest.netProxy = this;

    this.onreadystatechange = this.XMLHttpRequest.onreadystatechange;
    this.ontimeout = this.XMLHttpRequest.ontimeout;

    this.readyState = 0;
    this.responseBody = '';
    this.responseText = '';
    this.responseXML = '';
    this.status = 0;
    this.statusText = '';
    this.timeout = null;
	
    this.abort = function(){
	    this.XMLHttpRequest.abort.apply(this.XMLHttpRequest, arguments);
    };

    this.getResponseHeader = function(){
	    this.XMLHttpRequest.this.getResponseHeader.apply(this.XMLHttpRequest, arguments);
    };

    this.getAllResponseHeaders = function(){
	    this.XMLHttpRequest.this.getAllResponseHeaders.apply(this.XMLHttpRequest, arguments);
    };

    this.open = function(){
	    this.XMLHttpRequest.onreadystatechange = function(){
            this.netProxy.readyState = this.readyState;
		    this.netProxy.responseBody = this.responseBody;
		    this.netProxy.responseText = this.responseText;
		    this.netProxy.responseXML = this.responseXML;
		    this.netProxy.status = this.status;
		    this.netProxy.statusText = this.statusText;
		    this.netProxy.onreadystatechange();
	    };
	    this.XMLHttpRequest.ontimeout = function(){
            this.netProxy.readyState = this.readyState;
            this.netProxy.responseBody = this.responseBody;
            this.netProxy.responseText = this.responseText;
            this.netProxy.responseXML = this.responseXML;
            this.netProxy.status = this.status;
            this.netProxy.statusText = this.statusText;
            this.netProxy.ontimeout();
	    };

	    if (arguments.length > 1)
		    arguments[1] = this.baseURL + '?' + escape(arguments[1]);
	    this.XMLHttpRequest.open.apply(this.XMLHttpRequest, arguments);
    };

    this.send = function(){
        if (this.timeout != null)
            this.XMLHttpRequest.timeout = this.timeout;
	    this.XMLHttpRequest.send.apply(this.XMLHttpRequest, arguments);
    };

    this.setRequestHeader = function(){
	    this.XMLHttpRequest.setRequestHeader.apply(this.XMLHttpRequest, arguments);
    };
}