<?php
   //$to = "zaihao90@hotmail.com"; // <– replace with your address here
  // $subject = "Test mail";
  // $message = "Forget you Password? Click here to Reset Password";
   //$from = "webcity.online@webcity.online";
  // $headers = "From:" . $from;
 //  mail($to,$subject,$message,$headers);
 //  echo "Mail Sent.";


//$headers .= "Reply-To: ". strip_tags($_POST['req-email']) . "\r\n";
//$headers .= "CC: susan@example.com\r\n";

   $to = 'zaihao90@hotmail.com';

$subject = 'WebCity Forget Password';

$headers = "From: webcity.online@webcity.online" . "\r\n";

$headers .= "MIME-Version: 1.0\r\n";
$headers .= "Content-Type: text/html; charset=UTF-8\r\n";

$message = '<p><strong>This is strong text</strong> while this is not.</p>';


mail($to, $subject, $message, $headers);
?>