export default function ValidationErrors({ errors }) {
  if (!errors || Object.keys(errors).length === 0) return null;

  return (
    <div className="validation-errors">
      <h4>Validation Errors</h4>
      {Object.entries(errors).map(([key, messages]) => (
        <div key={key} className="error-group">
          <code className="error-key">{key}</code>
          <ul>
            {messages.map((msg, i) => (
              <li key={i}>{msg}</li>
            ))}
          </ul>
        </div>
      ))}
    </div>
  );
}
