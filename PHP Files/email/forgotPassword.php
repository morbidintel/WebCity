<html>
<head>
	<title>Forgotten Password</title>
	<meta charset="utf-8" />
	<meta name="viewport" content="width=device-width, initial-scale=1" />
	<link rel="stylesheet" href="assets/css/main.css" />
	<script src='https://www.google.com/recaptcha/api.js'></script>
</head>
<body>

	<!-- Header -->
	<header id="header">
		<div class="inner">
			<a href="index.html" class="logo">Theory</a>
			<a href="#navPanel" class="navPanelToggle"><span class="fa fa-bars"></span></a>
		</div>
	</header>

	<!-- Banner -->
	<section id="banner">
		<h1>WebCity Online</h1>
		<p>Reset Password Page</p>
	</section>

	<!-- One -->
	<section id="one" class="wrapper">
		<div class="inner">
			<div class="flex flex-3">
				<form method="post" action="/email/changepassword.php">
					<div class="6u$ 12u$(small)">
					
					</div>
					<div class="row uniform">
						<div class="12u$">
							<div class="6u 12u$(xsmall)">
								<input type="text" name="newPassword" id="newPassword" value="" placeholder="New Password" />
							</div>
						</div>
						<!-- Break -->
						<div class="12u$">
							<div class="6u 12u$(xsmall)">
								<input type="text" name="confirmPassword" id="confirmPassword" value="" placeholder="Confirm Password" />
							</div>
						</div>
							<?php
								$userid = $_GET['userid'];
							?>
							<input type="hidden" name="userid" id="userid" value="<?php echo $userid;?>" />

						<!-- Break -->

						<div class="6u$ 12u$(small)">
							<input type="checkbox" id="human" name="human" checked>
							<div class="g-recaptcha" data-sitekey="6LeaGl8UAAAAAC-VPiezjTGl5zqO9rfeWzM6xHr5"></div>
						</div>

						<!-- Break -->
						<div class="12u$">
							<ul class="actions">
								<!--<li><a href="#" class="button special" onclick="validateForm()">Submit</a></li>-->
								
								<li><input type="submit" name="submit" id="submit"></li>
							</ul>
						</div>
					</div>
				</form>
			</div>
		</div>
	</section>



	<!-- Footer -->
	<footer id="footer">
		<div class="inner">
			<div class="flex">
				<div class="copyright">
					&copy;<a href="">Webcity Online</a>.
				</div>
				<ul class="icons">
					<li><a href="#" class="icon fa-facebook"><span class="label">Facebook</span></a></li>
					<li><a href="#" class="icon fa-twitter"><span class="label">Twitter</span></a></li>
					<li><a href="#" class="icon fa-linkedin"><span class="label">linkedIn</span></a></li>
					<li><a href="#" class="icon fa-pinterest-p"><span class="label">Pinterest</span></a></li>
					<li><a href="#" class="icon fa-vimeo"><span class="label">Vimeo</span></a></li>
				</ul>
			</div>
		</div>
	</footer>

	<!-- Scripts -->
	<script src="assets/js/jquery.min.js"></script>
	<script src="assets/js/skel.min.js"></script>
	<script src="assets/js/util.js"></script>
	<script src="assets/js/main.js"></script>
		
</body>
</html>
	<?php
		
	?>