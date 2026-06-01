function navigateToPage(file) { window.location.href = file; }

document.addEventListener('DOMContentLoaded', function() {
  document.querySelectorAll('.comment-submit').forEach(function(btn) {
    btn.addEventListener('click', function() {
      var blockId = btn.getAttribute('data-block');
      var section = document.querySelector('[data-comments-block="' + blockId + '"]');
      if (!section) return;
      var author = section.querySelector('.comment-author').value.trim() || 'Гость';
      var text = section.querySelector('.comment-text').value.trim();
      if (!text) return;
      var list = section.querySelector('.comments-list');
      var article = document.createElement('article');
      article.className = 'comment-item';
      article.innerHTML = '<strong>' + escapeHtml(author) + '</strong><p>' + escapeHtml(text) + '</p>';
      list.appendChild(article);
      section.querySelector('.comment-text').value = '';
      var key = 'cms_comments_' + blockId;
      var stored = JSON.parse(localStorage.getItem(key) || '[]');
      stored.push({ author: author, text: text, at: new Date().toISOString() });
      localStorage.setItem(key, JSON.stringify(stored));
      syncCommentToViewers(blockId, author, text);
    });
  });
  document.querySelectorAll('[data-comments-block]').forEach(function(section) {
    var blockId = section.getAttribute('data-comments-block');
    var key = 'cms_comments_' + blockId;
    var stored = JSON.parse(localStorage.getItem(key) || '[]');
    var list = section.querySelector('.comments-list');
    stored.forEach(function(c) {
      var article = document.createElement('article');
      article.className = 'comment-item';
      article.innerHTML = '<strong>' + escapeHtml(c.author) + '</strong><p>' + escapeHtml(c.text) + '</p>';
      list.appendChild(article);
      syncCommentToViewers(blockId, c.author, c.text);
    });
  });
});
function syncCommentToViewers(blockId, author, text) {
  document.querySelectorAll('[data-comments-viewer][data-comments-source="' + blockId + '"]').forEach(function(viewer) {
    var list = viewer.querySelector('.comments-list');
    if (!list) return;
    var empty = list.querySelector('.comments-empty');
    if (empty) empty.remove();
    var article = document.createElement('article');
    article.className = 'comment-item';
    article.innerHTML = '<strong>' + escapeHtml(author) + '</strong><p>' + escapeHtml(text) + '</p>';
    list.appendChild(article);
  });
}
function escapeHtml(s) {
  return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function collectFormData(formId) {
  var form = document.getElementById('el-' + formId);
  if (!form) return {};
  var data = {};
  form.querySelectorAll('input, textarea, select').forEach(function(el) {
    if (el.name) data[el.name] = el.type === 'checkbox' ? el.checked : el.value;
  });
  document.querySelectorAll('[data-form-id="' + formId + '"]').forEach(function(el) {
    if (el.name && !(el.name in data)) data[el.name] = el.type === 'checkbox' ? el.checked : el.value;
  });
  return data;
}
function submitForm_13(ev) {
  if (ev) ev.preventDefault();
  var data = collectFormData(13);
  console.log('Form submit', data);
  alert('Форма отправлена!\n' + JSON.stringify(data, null, 2));
}
