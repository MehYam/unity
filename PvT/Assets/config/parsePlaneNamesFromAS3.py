import fileinput
import re

for line in fileinput.input():
	if 'symbol=\"ship' in line:
		name = re.search(r'"ship.*"', line)
		print(name.group().replace('"', ''))
