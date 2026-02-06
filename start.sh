#!/bin/bash
# Start script for Render.com
cd out
dotnet Joja.Api.dll --urls "http://0.0.0.0:${PORT:-5000}"
