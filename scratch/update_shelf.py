import re

with open('src/Vargshala.Web/wwwroot/images/online-learning.svg', 'r', encoding='utf-8') as f:
    text = f.read()

start_marker = '<rect x="55.16" y="127.49"'
end_marker = '<path d="M442.88,319.65H277.47V45.51H442.88Z'

start_pos = text.find(start_marker)
end_pos = text.find(end_marker)

print(f"Start pos: {start_pos}, End pos: {end_pos}")
original_shelf_chunk = text[start_pos:end_pos]
print("Original shelf chunk:\n", original_shelf_chunk)
