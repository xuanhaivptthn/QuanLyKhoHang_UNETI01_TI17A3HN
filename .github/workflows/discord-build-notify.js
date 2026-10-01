const webhookUrl = process.env.DISCORD_WEBHOOK;
if (!webhookUrl) {
  console.log('DISCORD_WEBHOOK secret is not set. Skipping build status notification.');
  process.exit(0);
}

const buildStatus = (process.env.BUILD_STATUS || '').toLowerCase();
const isSuccess = buildStatus === 'success';

let author = process.env.AUTHOR || '';
const branch = process.env.BRANCH || '';
const repo = process.env.REPO || '';
let sha = process.env.SHA || '';
let commitMsg = process.env.COMMIT_MESSAGE || '';
const runId = process.env.RUN_ID || '';
const serverUrl = process.env.SERVER_URL || 'https://github.com';

// Fallback to head commit JSON if needed
if ((!commitMsg || !sha || !author) && process.env.HEAD_COMMIT_JSON) {
  try {
    const head = JSON.parse(process.env.HEAD_COMMIT_JSON);
    if (!commitMsg && head.message) commitMsg = head.message;
    if (!sha && head.id) sha = head.id;
    if (!author && head.author) author = head.author.username || head.author.name || '';
  } catch (e) {
    // Ignore JSON parse error
  }
}

const shortSha = sha ? sha.substring(0, 7) : '';
const firstLineMsg = commitMsg ? commitMsg.trim().split('\n')[0] : 'No commit message';
const runUrl = runId ? `${serverUrl}/${repo}/actions/runs/${runId}` : `${serverUrl}/${repo}/actions`;
const commitUrl = sha ? `${serverUrl}/${repo}/commit/${sha}` : `${serverUrl}/${repo}`;

const statusTitle = isSuccess ? 'Build Succeeded' : 'Build Failed';
const statusEmoji = isSuccess ? '✅' : '❌';
// 3066993 = green (#2ecc71), 15158332 = red (#e74c3c)
const embedColor = isSuccess ? 3066993 : 15158332;

// Note: No role mention in content so the role is not pinged for build status
const payload = {
  embeds: [
    {
      title: `${statusEmoji} ${statusTitle}`,
      url: runUrl,
      color: embedColor,
      author: {
        name: author || 'GitHub Actions',
        url: author ? `https://github.com/${author}` : undefined,
        icon_url: author ? `https://github.com/${author}.png` : undefined
      },
      description: shortSha
        ? `Commit [\`${shortSha}\`](${commitUrl}): ${firstLineMsg}`
        : firstLineMsg,
      fields: [
        {
          name: 'Repository',
          value: `[${repo}](${serverUrl}/${repo})`,
          inline: true
        },
        {
          name: 'Branch',
          value: `\`${branch}\``,
          inline: true
        },
        {
          name: 'Status',
          value: isSuccess ? 'Success' : 'Failure',
          inline: true
        }
      ],
      footer: {
        text: `GitHub Actions • Run #${runId}`
      },
      timestamp: new Date().toISOString()
    }
  ]
};

async function sendNotification() {
  try {
    const res = await fetch(webhookUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'User-Agent': 'GitHub-Actions-Discord-Build-Notifier'
      },
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const errText = await res.text();
      console.error(`Discord API responded with status ${res.status}: ${errText}`);
      process.exit(1);
    }

    console.log(`Successfully notified Discord: ${statusTitle}.`);
  } catch (err) {
    console.error('Failed to send build status notification to Discord:', err);
    process.exit(1);
  }
}

sendNotification();
