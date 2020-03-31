#!/bin/bash

function ensure_success {
	exit_status=$?
	if [ $exit_status -ne 0 ]; then
		echo "... command exited with code $exit_status"
		exit $exit_status
	fi
}

curl -sS https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add -
ensure_success

echo "deb https://dl.yarnpkg.com/debian/ stable main" | tee /etc/apt/sources.list.d/yarn.list
ensure_success

apt update && apt install --no-install-recommends yarn
ensure_success
