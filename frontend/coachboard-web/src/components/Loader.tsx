type LoaderProps = {
  size?: 'sm' | 'md' | 'lg'
  text?: string
}

export default function Loader({ size = 'md', text }: LoaderProps) {
  const sizeClasses = {
    sm: 'w-4 h-4 border-2',
    md: 'w-8 h-8 border-2',
    lg: 'w-12 h-12 border-4',
  }

  return (
    <div className="flex flex-col items-center justify-center gap-2 py-8">
      <div
        className={`${sizeClasses[size]} animate-spin rounded-full border-primary-600 border-t-transparent`}
      />
      {text && <p className="text-sm text-gray-500">{text}</p>}
    </div>
  )
}

