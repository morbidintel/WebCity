mergeInto(LibraryManager.library, {
	
	OpenLink: function (url) {
		var str = Pointer_stringify(url);
		document.onmouseup = function()
		{
			window.open(str);
			document.onmouseup = null;
		}
	},

});