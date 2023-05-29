namespace WebImageLibPoc.Pages
{
    public partial class Index
    {
        private long _prevNumber = 1;
        private long _nextNumber;
        private long _fibonacciNumber;
        private bool _isIncrement = true;

        //I know Fibonacci is incremental only, not decremental
        //This can be solved via pushing a stack when we increment,
        //but for demo only I want to make it algebraic
        private void Decrement()
        {
            if (_fibonacciNumber <= 0)
            {
                Console.WriteLine("Cant decrement anymore");
                return;
            }

            if (_isIncrement)
            {
                _fibonacciNumber = _prevNumber;
                _isIncrement = false;
                return;
            }

            var computed = _nextNumber - _prevNumber;
            _nextNumber = _prevNumber;
            _prevNumber = computed;
            _fibonacciNumber = computed;
        }

        private void Increment()
        {
            if (!_isIncrement)
            {
                _fibonacciNumber = _nextNumber;
                _isIncrement = true;
                return;
            }

            var computed = _prevNumber + _nextNumber;
            _prevNumber = _nextNumber;
            _nextNumber = computed;
            _fibonacciNumber = computed;
        }
    }
}