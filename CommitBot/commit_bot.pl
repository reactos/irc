#!/usr/bin/perl -w
#
# File:    commit_bot.pl
# Author:  Pierre Schweitzer <pierre@reactos.org>
# Created: 29-Sep-12
# Licence: GNU GPL v2 or any later version
# Purpose: IRC bot to announce commits to IRC
#          It has been specifically designed for Freenode network
#          Freely inspired from http://www.javalinux.it/wordpress/2009/10/15/writing-an-irc-bot-for-svn-commit-notification/
#

my $server = "irc.freenode.net";
my $port = 6667;
my $nick = "".int(rand(100));
my $ident = "";
my $realname = "";
my @chans = ("");
my $pass = "";
my $commit_repo = $ARGV[0];
my $commit_rev = $ARGV[1];
my $commit_author = $ARGV[2];
my $commit_msg = $ARGV[3];
my $state = 0;
my $lines = 0;
use IO::Socket;

my $irc=IO::Socket::INET->new(
   PeerAddr => $server,
   PeerPort => $port,
   Proto => 'tcp'
) or die "Failure!";

if ($pass !~ /^$/) {
   print $irc "PASS $pass\n";
}
print $irc "USER $nick $ident $ident :$realname\r\n";
print $irc "NICK $nick\r\n";

while (my $in = <$irc>) {
   if ($in =~ /376 $nick/)
   {
      foreach my $chan (@chans) {
         print $irc "JOIN $chan\n";
      }
      $state = 1;
   }
   elsif ($in =~ /^:(.*)!.* PRIVMSG $nick :.*VERSION.*/) {
      print $irc "NOTICE $1 :".chr(1)."VERSION RosCommit 0.1".chr(1)."\n";
   }
   elsif($in =~ /JOIN/i && $state == 1) {
      $state = 2;
      sleep 1;
      foreach my $chan (@chans) {
         print $irc "PRIVMSG $chan :".chr(3)."3$commit_author".chr(15)." $commit_repo ".chr(2)."r$commit_rev".chr(2).":\n";
      }
   }
   elsif ($in =~ /^PING(.*)$/i) {
      print $irc "PONG :$1\n";
   }
   elsif ($state == 2) {
      my $lines = 0;
      while ($commit_msg !~ /^$/ && $lines < 6) {
         $commit_msg =~ s/^(.{0,500})//;
         my $chunk = $1;
         $commit_msg =~ s/^\R+//;
         $lines++;
         foreach my $chan (@chans) { 
            print $irc "PRIVMSG $chan :$chunk\n";
         }
         select(undef, undef, undef, 0.5);
      }
      sleep 1;
      last;
   }
}
print $irc "QUIT :Done\n";
sleep 1;
close($irc);
