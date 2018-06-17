<?php
	// output error in json and exit
	function error($msg)
	{
		echo "{\"error\": \"{$msg}\"}";
		$mysqli->close();
		exit();
	}
?>