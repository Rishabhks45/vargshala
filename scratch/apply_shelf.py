import os

shelf_svg = '''<!-- Enhanced Shelf matching User Image 1 -->
<g id="freepik--Shelf-and-Books">
    <!-- Shelf Board (Mint/Teal) -->
    <rect x="55.16" y="127.49" width="125" height="7.42" rx="1.5" style="fill: #99E2D8;" id="el8us32mxguj6" class="animable"></rect>
    <!-- Left Bracket (Mint/Teal) -->
    <rect x="64.65" y="134.91" width="7.43" height="24.55" rx="1" style="fill: #80D8CB;" id="elmkv2g33y1ke" class="animable"></rect>
    <rect x="64.65" y="134.91" width="7.43" height="4.5" style="fill: #6EC6B8;" id="elv537dpwovm" class="animable"></rect>

    <!-- Books Group on the Shelf -->
    <g id="shelf-books">
        <!-- Book 1 (Dark Deep Teal) -->
        <g id="shelf-book-1">
            <rect x="79.5" y="76" width="15" height="51.5" rx="1" style="fill: #004D40;"></rect>
            <!-- Spine detail stripes -->
            <rect x="79.5" y="86" width="15" height="3" style="fill: #ffffff; opacity: 0.9;"></rect>
            <rect x="79.5" y="92" width="15" height="2" style="fill: #ffffff; opacity: 0.75;"></rect>
            <rect x="79.5" y="116" width="15" height="3" style="fill: #ffffff; opacity: 0.9;"></rect>
            <rect x="79.5" y="121" width="15" height="2" style="fill: #ffffff; opacity: 0.75;"></rect>
        </g>

        <!-- Book 2 (Medium Institute Teal) -->
        <g id="shelf-book-2">
            <rect x="94.5" y="68" width="18" height="59.5" rx="1" style="fill: #00796B;"></rect>
            <!-- White stripes & spine band -->
            <rect x="94.5" y="78" width="18" height="3.5" style="fill: #ffffff; opacity: 0.95;"></rect>
            <rect x="94.5" y="83.5" width="18" height="2" style="fill: #ffffff; opacity: 0.8;"></rect>
            <rect x="94.5" y="113" width="18" height="3.5" style="fill: #ffffff; opacity: 0.95;"></rect>
            <rect x="94.5" y="118.5" width="18" height="2" style="fill: #ffffff; opacity: 0.8;"></rect>
        </g>

        <!-- Book 3 (Tallest Teal with White Bookmark/Label) -->
        <g id="shelf-book-3">
            <rect x="112.5" y="62" width="14" height="65.5" rx="1" style="fill: #26A69A;"></rect>
            <!-- White bookmark / spine label (exact as in user image) -->
            <rect x="116.5" y="68" width="6" height="28" rx="1" style="fill: #ffffff;"></rect>
        </g>

        <!-- Book 4 (Soft Light Teal) -->
        <g id="shelf-book-4">
            <rect x="126.5" y="76" width="19" height="51.5" rx="1" style="fill: #4DB6AC;"></rect>
            <!-- Horizontal white stripes near bottom -->
            <rect x="126.5" y="110" width="19" height="3.5" style="fill: #ffffff; opacity: 0.95;"></rect>
            <rect x="126.5" y="116" width="19" height="2" style="fill: #ffffff; opacity: 0.8;"></rect>
        </g>
    </g>

    <!-- Potted Plant on the Right of Shelf (exact as in user image 1) -->
    <g id="shelf-potted-plant">
        <!-- Plant Leaves (Deep Teal & Vibrant Teal) -->
        <g id="plant-leaves">
            <!-- Center upright leaf -->
            <path d="M160 106 C157 90, 156 74, 160 62 C164 74, 163 90, 160 106 Z" style="fill: #004D40;"></path>
            <!-- Center leaf inner highlight -->
            <path d="M160 104 C158 92, 158 80, 160 70 C162 80, 162 92, 160 104 Z" style="fill: #00796B; opacity: 0.7;"></path>

            <!-- Mid left leaf -->
            <path d="M158 106 C150 92, 147 80, 151 68 C157 78, 160 92, 158 106 Z" style="fill: #00796B;"></path>
            <!-- Mid right leaf -->
            <path d="M162 106 C170 92, 173 80, 169 68 C163 78, 160 92, 162 106 Z" style="fill: #00695C;"></path>

            <!-- Lower left leaf -->
            <path d="M156 107 C144 98, 140 89, 144 80 C151 88, 156 97, 156 107 Z" style="fill: #004D40;"></path>
            <!-- Lower right leaf -->
            <path d="M164 107 C176 98, 180 89, 176 80 C169 88, 164 97, 164 107 Z" style="fill: #00796B;"></path>

            <!-- Leaf stems/base -->
            <rect x="158" y="104" width="4" height="4" style="fill: #004D40;"></rect>
        </g>

        <!-- Pot Rim -->
        <rect x="148" y="104" width="24" height="5.5" rx="1.5" style="fill: #4DB6AC;"></rect>
        <rect x="148" y="108" width="24" height="1.5" style="fill: #004D40; opacity: 0.35;"></rect>

        <!-- Pot Body (Trapezoid) -->
        <polygon points="150,109.5 170,109.5 167,127.49 153,127.49" style="fill: #80CBC4;"></polygon>
        <!-- Pot Shadow / Detail -->
        <polygon points="150,109.5 156,109.5 154.5,127.49 153,127.49" style="fill: #4DB6AC; opacity: 0.6;"></polygon>
    </g>
</g>
'''

start_marker = '<rect x="55.16" y="127.49"'
end_marker = '<path d="M442.88,319.65H277.47V45.51H442.88Z'

targets = [
    'src/Vargshala.Web/wwwroot/images/online-learning.svg',
    'src/Vargshala.Web/wwwroot/animated svg/online-learning.svg'
]

for t in targets:
    if not os.path.exists(t):
        print(f"File not found: {t}")
        continue
    with open(t, 'r', encoding='utf-8') as f:
        content = f.read()
    
    start_pos = content.find(start_marker)
    end_pos = content.find(end_marker)
    
    if start_pos == -1 or end_pos == -1:
        print(f"Markers not found in {t}: start={start_pos}, end={end_pos}")
        continue
    
    new_content = content[:start_pos] + shelf_svg.strip() + '\n' + content[end_pos:]
    with open(t, 'w', encoding='utf-8') as f:
        f.write(new_content)
    print(f"Successfully updated {t}!")
