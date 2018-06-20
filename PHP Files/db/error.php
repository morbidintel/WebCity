<?php
	// output error in json and exit
	function error($msg)
	{
		echo "{\"error\": \"{$msg}\"}";
		if (isset($mysql)) $mysqli->close();
		exit();
	}
?>