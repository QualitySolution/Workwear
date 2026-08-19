#!/bin/bash
cd "$(dirname "$0")/Workwear/bin/Debug" || exit 1

# Mono 6.12 cannot read modern terminfo entries larger than 4 KiB.
# openSUSE's xterm-256color entry is larger and breaks System.Console.
export TERM=screen-256color

exec mono workwear.exe "$@"
