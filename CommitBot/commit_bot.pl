#!/usr/bin/perl
#
# File:    commit_bot.pl
# Author:  Pierre Schweitzer <pierre@reactos.org>
# Created: 29-Sep-12
# Licence: GNU GPL v2 or any later version
# Purpose: IRC bot to announce commits to IRC
#          It has been specifically designed for Freenode network
#

use warnings;
use strict;
use encoding 'utf8';
use Time::HiRes;

# Env
my $svnlook = "/usr/bin/svnlook";

# Settings
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

# Gather information about the commit
my $commit_author = `$svnlook author -r $commit_rev $repo`;
my $commit_files = `$svnlook changed -r $commit_rev $repo | wc -l`;
my $commit_dirs = `$svnlook dirs-changed -r $commit_rev $repo | wc -l`;
my $commit_msg = `$svnlook log -r $commit_rev $repo`;
my $commit_branch = "*";

# Get the common path between all the changed
my @dirs_list = split(/\R/, `$svnlook dirs-changed -r $commit_rev $repo`);
my $commit_common = make_min_path(@dirs_list);

# Remove the final \n
chomp($commit_author);
chomp($commit_files);
chomp($commit_dirs);
chomp($commit_msg);

# If we have a few files, try to display them
if ($commit_files <= 4) {
   # Keep file count
   my $fc = 0;
   my $commit_common_len = length($commit_common);
   $commit_files = "";
   # Get files list
   my @files_list = split(/\R/,  `$svnlook changed -r $commit_rev $repo | cut -d" " --complement -f1`);
   # Delete the common path in them
   foreach my $file (@files_list) {
      $file =~ s/^\s+//;
      $commit_files .= ($fc > 0 ? " " : "") . substr($file, $commit_common_len);
      $fc++;
   }

   # If it cannot be disabled, fall back to files number
   if (length($commit_files) > 400) {
      $commit_files = $fc;
   } else {
      $details = 1;
   }
}

# Now, handle specific path and remove them
if ($commit_common =~ /^branches\//) {
   my $next = index $commit_common, "/", 9;
   $commit_branch = chr(3)."7".substr($commit_common, 9, $next - 9).chr(15);
   $commit_common = substr($commit_common, $next);
} elsif ($commit_common =~ /^tags\//) {
   my $next = index $commit_common, "/", 5;
   $commit_branch = chr(3)."7".substr($commit_common, 5, $next - 5).chr(15);
   $commit_common = substr($commit_common, $next);
} elsif ($commit_common =~ /^trunk\//) {
   $commit_common =~ s/^.{6}//;
   # Also color first dir, as CIA did
   $commit_common = chr(3)."10$commit_common";
   my $pos = index $commit_common, "/";
   my $begin = substr $commit_common, 0, $pos;
   my $end = substr $commit_common, $pos;
   $commit_common = $begin.chr(15).$end;
}

use IO::Socket;

# Connect to IRC
my $irc=IO::Socket::INET->new(
   PeerAddr => $server,
   PeerPort => $port,
   Proto => 'tcp'
) or die "Failure!";

# Auth if password provided
if ($pass !~ /^$/) {
   print $irc "PASS $pass\n";
}
# Send connection data
print $irc "USER $nick $ident $ident :$realname\r\n";
print $irc "NICK $nick\r\n";

# Switch to Unicode
binmode($irc, ":encoding(UTF-8)");

while (my $in = <$irc>) {
   # On end of MOTD, join chans
   if ($in =~ /376 $nick/)
   {
      foreach my $chan (@chans) {
         print $irc "JOIN $chan\n";
      }
      $state = 1;
   }
   # frigg is likely to ask for version, politely answer
   elsif ($in =~ /^:(.*)!.* PRIVMSG $nick :.*VERSION.*/) {
      print $irc "NOTICE $1 :".chr(1)."VERSION RosCommit 0.3".chr(1)."\n";
   }
   # Once confirmation about joins is OK, start sending header
   elsif($in =~ /JOIN/i && $state == 1) {
      $state = 2;
      sleep 1;
      my $msg;
      if ($details == 1) {
         $msg = chr(2)."$commit_repo: ".chr(15).chr(3)."3$commit_author".chr(15)." $commit_branch ".chr(2)."r$commit_rev".chr(15)." $commit_common ($commit_files):\n";
      } else {
         $msg = chr(2)."$commit_repo: ".chr(15).chr(3)."3$commit_author".chr(15)." $commit_branch ".chr(2)."r$commit_rev".chr(15)." $commit_common ($commit_files file".($commit_files <= 1 ? "" : "s")." in $commit_dirs dir".($commit_dirs <= 1 ? "" : "s")."):\n";
      }
      foreach my $chan (@chans) {
         print $irc "PRIVMSG $chan :$msg";
      }
   }
   # Answer to server PING
   elsif ($in =~ /^PING(.*)$/i) {
      print $irc "PONG :$1\n";
   }
   # The room is ready for commit info
   elsif ($state == 2) {
      my $lines = 0;
      # Display only lines of 500 max chars at once
      while ($commit_msg !~ /^$/ && $lines < 6) {
         $commit_msg =~ s/^(.{0,500})//;
         my $chunk = $1;
         $commit_msg =~ s/^\R+//;
         $lines++;
         foreach my $chan (@chans) {
            print $irc "PRIVMSG $chan :".chr(2)."$commit_repo: ".chr(15)."$chunk\n";
         }
         sleep(0.5);
      }
      sleep 1;
      last;
   }
}
print $irc "QUIT :Done\n";
sleep 1;
close($irc);

sub min {
   my $a = shift(@_);
   my $b = shift(@_);
   return ($a > $b) ? $b : $a;
}

# Given a list of path, return the longest common path possible
sub make_min_path {
   my @paths = @_;
   # Suppose that first path is the longest
   my $common = $paths[0];
   my $last = 0;

   # Check with all paths
   foreach my $path (@paths) {
      my $new_length = 0;
      my $min_length = min(length($path), length($common));
      my $pos = 0;

      # Check how far they match
      while ($pos < $min_length) {
         last if substr($common, $pos, 1) ne substr($path, $pos, 1);
         $last = ($pos + 1) if substr($common, $pos, 1) eq "/";
         $pos++;
      }
      # Then keep it as new longest common path
      $common = substr($common, 0, $pos);
   }

   # Chomp till last "/" to prevent partial paths
   $common = substr($common, 0, $last);

   return $common;
}
