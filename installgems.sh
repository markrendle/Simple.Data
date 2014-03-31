#!/bin/bash

if ! which bundle > /dev/null; then
  echo "*** Installing Bundler"
  gem install bundler --no-rdoc --no-ri
fi

bundle install
