#!/bin/bash

MY="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "App prereqs"
$MY/install-app-prereqs.sh

echo "Chromium"
$MY/install-chromium.sh

echo "Node.js"
$MY/install-nodejs.sh

echo "Yarn"
$MY/install-yarn.sh