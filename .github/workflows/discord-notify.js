const webhookUrl = process.env.DISCORD_WEBHOOK;
if (!webhookUrl) {
  console.log('DISCORD_WEBHOOK secret is not set. Skipping notification.');
  process.exit(0);
}

const roleId = process.env.DISCORD_ROLE_ID;
const author = process.env.AUTHOR || '';
const branch = process.env.BRANCH || '';
const repo = process.env.REPO || '';
const compareUrl = process.env.COMPARE_URL || '';

let commits = [];
try {
  commits = JSON.parse(process.env.COMMITS_JSON || '[]');
} catch (e) {
  commits = [];
}

// Fallback to head commit if commits array is empty (e.g., tag push)
if (!Array.isArray(commits) || commits.length === 0) {
  const headRaw = process.env.HEAD_COMMIT_JSON;
  if (headRaw) {
    try {
      commits = [JSON.parse(headRaw)];
    } catch (e) {
      commits = [];
    }
  }
}

const commitCount = commits.length;
const plural = commitCount <= 1 ? 'commit' : 'commits';

const mention = roleId ? `<@&${roleId}> ` : '';
const content = `${mention}**${commitCount}** new ${plural} pushed to **${repo}** (\`${branch}\`)`;

const commitLines = commits.map(c => {
  const sha = (c.id || '').substring(0, 7);
  const url = c.url || '';
  const msg = (c.message || '').trim().split('\n')[0];
  const committer = (c.author && c.author.name) || author;
  return `[\`${sha}\`](${url}) ${msg} - *${committer}*`;
});

let description = commitLines.join('\n');
if (description.length > 4000) {
  description = description.substring(0, 3950) + '\n... (truncated)';
}

const fallbackUrl = commits.length > 0 ? commits[commits.length - 1].url : `https://github.com/${repo}`;

const payload = {
  content: content,
  embeds: [
    {
      title: `Compare changes (${commitCount} ${plural})`,
      url: compareUrl || fallbackUrl,
      description: description,
      color: 3447003,
      author: {
        name: author,
        url: `https://github.com/${author}`,
        icon_url: `https://github.com/${author}.png`
      },
      fields: [
        {
          name: 'Branch',
          value: `\`${branch}\``,
          inline: true
        }
      ]
    }
  ]
};

async function sendNotification() {
  try {
    const res = await fetch(webhookUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'User-Agent': 'GitHub-Actions-Discord-Notifier'
      },
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const errText = await res.text();
      console.error(`Discord API responded with status ${res.status}: ${errText}`);
      process.exit(1);
    }

    console.log(`Successfully notified Discord for ${commitCount} ${plural}.`);
  } catch (err) {
    console.error('Failed to send notification to Discord:', err);
    process.exit(1);
  }
}

sendNotification();
