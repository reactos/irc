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

use encoding 'utf8';

my $svnlook = "/usr/bin/svnlook";

my $server = "irc.freenode.net";
my $port = 6667;
my $nick = "".int(rand(100));
my $ident = "";
my $realname = "";
my @chans = ("");
my $pass = "";
my $repo = $ARGV[0];
my $commit_rev = $ARGV[1];
my $commit_repo = $ARGV[2];
my $state = 0;
my $lines = 0;
my $details = 0;

my $commit_author = `$svnlook author -r $commit_rev $repo`;
my $commit_files = `$svnlook changed -r $commit_rev $repo | wc -l`;
my $commit_dirs = `$svnlook dirs-changed -r $commit_rev $repo | wc -l`;
my $commit_msg = `$svnlook log -r $commit_rev $repo`;

chomp($commit_author);
chomp($commit_files);
chomp($commit_dirs);
chomp($commit_msg);

if ($commit_dirs == 1 && $commit_files <= 4) {
   $commit_files = `$svnlook changed -r $commit_rev $repo | awk '{print \$2}' | xargs basename`;
   $commit_dirs = `$svnlook dirs-changed -r $commit_rev $repo`;

   chomp($commit_files);
   chomp($commit_dirs);
   $commit_files =~ s/\R/ /g;

   $details = 1;
}

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

binmode($irc, ":utf8");

while (my $in = <$irc>) {
   if ($in =~ /376 $nick/)
   {
      foreach my $chan (@chans) {
         print $irc "JOIN $chan\n";
      }
      $state = 1;
   }
   elsif ($in =~ /^:(.*)!.* PRIVMSG $nick :.*VERSION.*/) {
      print $irc "NOTICE $1 :".chr(1)."VERSION RosCommit 0.2".chr(1)."\n";
   }
   elsif($in =~ /JOIN/i && $state == 1) {
      $state = 2;
      sleep 1;
      my $msg;
      if ($details == 1) {
         $msg = chr(2)."$commit_repo: ".chr(15).chr(3)."3$commit_author".chr(15)." * ".chr(2)."r$commit_rev".chr(15)." $commit_dirs ($commit_files):\n";
      } else {
         $msg = chr(2)."$commit_repo: ".chr(15).chr(3)."3$commit_author".chr(15)." * ".chr(2)."r$commit_rev".chr(15)." ($commit_files file".($commit_files <= 1 ? "" : "s")." in $commit_dirs dir".($commit_dirs <= 1 ? "" : "s")."):\n";
      }
      foreach my $chan (@chans) {
         print $irc "PRIVMSG $chan :$msg";
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
            print $irc "PRIVMSG $chan :".chr(2)."$commit_repo: ".chr(15)."$chunk\n";
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
