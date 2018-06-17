<?php
	/*
	Usage:
	if (($p = check_params('username', 'pwhash')) != null)
		error('Parameter '.$p.' not set');
	*/
	function check_params()
	{
		$args = func_get_args();
		foreach ($args as $arg)
		{
			if (!isset($_GET[$arg])) return $arg;
		}
		return null;
	}
?>