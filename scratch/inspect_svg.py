import re

with open('src/Vargshala.Web/wwwroot/images/online-learning.svg', 'r', encoding='utf-8') as f:
    text = f.read()

ids = re.findall(r'<g [^>]*id="([^"]+)"', text)
print("All group IDs:", ids[:30])

# Look for shelf / book / plant coordinates
# The shelf seen earlier: x="55.16" y="127.49" width="125" height="7.42"
shelf_match = re.search(r'.{0,200}127\.49.{0,400}', text)
if shelf_match:
    print("\nNear shelf (127.49):")
    print(shelf_match.group(0))
