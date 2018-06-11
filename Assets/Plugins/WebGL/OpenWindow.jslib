var OpenWindowPlugin = {    
	openFixedSizedWindow: function(link,wt,ht)
    {
		var maxw = (screen.width*2)/3;
		var maxh = (screen.height*2)/3;

		var width = wt;
		var height = ht;
		
		width = (width > maxw ? maxw : width);
		height = (height > maxh ? maxh : height);
		
		
    	var url = Pointer_stringify(link);
        document.onmouseup = function()
        {
			window.open(url,'popupwindow',"top=0,left=0,width="+width+ ",height="+height);
        	document.onmouseup = null;
        }
    }
};

mergeInto(LibraryManager.library, OpenWindowPlugin);