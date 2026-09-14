# 📚 Vargshala AI Chat History Archive

This directory contains permanent, searchable, Git-tracked exports of AI development sessions.
These transcripts preserve architecture decisions, bug fixes, SQL scripts, and feature discussions directly in the repository so they are never lost, can be pushed to GitHub, and can be read by both humans and AI agents anytime.

---

## 📁 Archived Sessions

| Session Date | File Name | Turns | Topics Covered |
| :--- | :--- | :---: | :--- |
| **2026-09-14** | [`2026-09-14_89bdcefb_chat.md`](./2026-09-14_89bdcefb_chat.md) | **122** | • Database backup with schema & seed data<br>• Attendance module debugging & fixes<br>• `OrgAdmin/Students.razor` gold standard verification<br>• Batch-Teacher assignment & multiple subjects<br>• 1-Month sample data seeding (attendance, sessions)<br>• Branch Admin isolation, controllers & authorization<br>• Branch fees, student fee lookup & payment allocation APIs |
| **2026-09-14** | [`2026-09-14_f995a616_chat.md`](./2026-09-14_f995a616_chat.md) | **2** | • Chat recovery & automatic repository export setup |

---

## ⚡ How to Export New / Future Sessions

Whenever you finish an AI session or want to capture the latest chat history to push to Git, run this command from the project root:

### Using PowerShell:
```powershell
.\scripts\export_chat.ps1
```

### Using Python:
```bash
python scripts/export_chat.py
```

This script will:
1. Scan local Antigravity transcripts in `%USERPROFILE%\.gemini\antigravity-ide\brain\`.
2. Extract all conversation turns, queries, and code actions.
3. Generate clean, searchable Markdown files in [`docs/chat_history/`](./).
4. Update this `README.md` index automatically.
